using System.Net.Http.Headers;
using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Business.Ultils;
using Server.Data;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using Microsoft.Extensions.Logging;
namespace Server.Business.Services;

public class SkinAnalyzeService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly IMapper _mapper;
    private readonly ILogger<SkinAnalyzeService> _logger;
    private readonly CloudianryService _cloudianryService;

    private readonly AISkinSetting _aiSkinSetting;
    private static readonly HttpClient HttpClient = new HttpClient();

    public SkinAnalyzeService(UnitOfWorks unitOfWorks, IMapper mapper, IOptions<AISkinSetting> aiSkinSetting, 
        ILogger<SkinAnalyzeService> logger, CloudianryService cloudianryService)
    {
        _unitOfWorks = unitOfWorks;
        _mapper = mapper;
        _logger = logger;
        _cloudianryService = cloudianryService;
        _logger = logger;
        _aiSkinSetting = aiSkinSetting.Value;
    }

    public async Task<SkinAnalyzeResponse> AnalyzeSkinAsync(IFormFile file, int userId)
    {
        if (file == null || file.Length == 0)
        {
            throw new BadRequestException("File cannot be null or empty.");
        }

        // Validate kích thước file <= 5MB
        const long maxFileSize = 5 * 1024 * 1024; // 5MB
        if (file.Length > maxFileSize)
        {
            throw new BadRequestException("File size must be less than 5MB.");
        }

        // Validate định dạng file (chỉ chấp nhận jpg, jpeg)
        var allowedExtensions = new[] { ".jpg", ".jpeg" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(fileExtension))
        {
            throw new BadRequestException("Only JPG and JPEG formats are allowed.");
        }

        var apiKey = _aiSkinSetting.ApiKey;

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream); 
        memoryStream.Position = 0;

        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, _aiSkinSetting.Url);
    
        request.Headers.Add("ailabapi-api-key", apiKey);
    
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(memoryStream), "image", file.FileName);
        content.Add(new StringContent(""), "face_quality_control");
        content.Add(new StringContent(""), "return_rect_confidence");
        content.Add(new StringContent(""), "return_maps");
    
        request.Content = content;

        try
        {
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStringAsync();
            var apiResult = JsonConvert.DeserializeObject<dynamic>(responseData);


            var test = apiResult.result.skin_type;
            // Map API response to SkinHealth entity
            var skinHealth = new SkinHealth
            {
                UserId = userId,
                Acne = GetApiResponseValue(apiResult.result.acne),
                SkinColor = GetApiResponseValue(apiResult.result.skin_color),
                SkinToneIta = GetApiResponseValue(apiResult.result.skintone_ita),
                SkinTone = GetApiResponseValue(apiResult.result.skin_tone),
                SkinHueHa = GetApiResponseValue(apiResult.result.skin_hue_ha),
                SkinAge = GetApiResponseValue(apiResult.result.skin_age),
                SkinType = GetApiResponseValue(apiResult.result.skin_type),
                LeftEyelids = GetApiResponseValue(apiResult.result.left_eyelids),
                RightEyelids = GetApiResponseValue(apiResult.result.right_eyelids),
                EyePouch = GetApiResponseValue(apiResult.result.eye_pouch),
                EyePouchSeverity = GetApiResponseValue(apiResult.result.eye_pouch_severity),
                DarkCircle = GetApiResponseValue(apiResult.result.dark_circle),
                ForeheadWrinkle = GetApiResponseValue(apiResult.result.forehead_wrinkle),
                CrowsFeet = GetApiResponseValue(apiResult.result.crows_feet),
                GlabellaWrinkle = GetApiResponseValue(apiResult.result.glabella_wrinkle),
                NasolabialFold = GetApiResponseValue(apiResult.result.nasolabial_fold),
                NasolabialFoldSeverity = GetApiResponseValue(apiResult.result.nasolabial_fold_severity),
                PoresForehead = GetApiResponseValue(apiResult.result.pores_forehead),
                PoresLeftCheek = GetApiResponseValue(apiResult.result.pores_left_cheek),
                PoresRightCheek = GetApiResponseValue(apiResult.result.pores_right_cheek),
                PoresJaw = GetApiResponseValue(apiResult.result.pores_jaw),
                BlackHead = GetApiResponseValue(apiResult.result.blackhead),
                Rectangle = GetApiResponseValue(apiResult.result.rectangle),
                Mole = GetApiResponseValue(apiResult.result.mole),
                ClosedComedones = GetApiResponseValue(apiResult.result.closed_comedones),
                SkinSpot = GetApiResponseValue(apiResult.result.skin_spot),
                FaceMaps = GetApiResponseValue(apiResult.result.face_maps),
                Sensitivity = GetApiResponseValue(apiResult.result.sensitivity),
                SensitivityArea = GetApiResponseValue(apiResult.result.sensitivity_area),
                SensitivityIntensity = GetApiResponseValue(apiResult.result.sensitivity_intensity),
                EyeFineLines = GetApiResponseValue(apiResult.result.eye_finelines),
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            // Save SkinHealth data to the database
            await _unitOfWorks.SkinHealthRepository.AddAsync(skinHealth);
            await _unitOfWorks.SkinHealthRepository.Commit();
            
            // upload image to cloudinary
            var uploadResult = await _cloudianryService.UploadImageAsync(file);
            
            var skinHealthImage = new SkinHealthImage
            {
                ImageUrl = uploadResult != null ? uploadResult.SecureUrl.ToString() : "",
                SkinHealthId = skinHealth.SkinHealthId,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };
            
            await _unitOfWorks.SkinHealthImageRepository.AddAsync(skinHealthImage);
            await _unitOfWorks.SkinHealthImageRepository.Commit();

            // Process skin concerns
            var skinConcerns = GetSkinConcerns(apiResult);

            // Retrieve skincare routines based on concerns
            var routines = await GetSkincareRoutinesAsync(skinConcerns);
            // Lấy danh sách các routine hiện tại của user
            var existingUserRoutines = await _unitOfWorks.UserRoutineRepository
                .FindByCondition(ur => ur.UserId == userId).ToListAsync();

            List<UserRoutine> userRoutinesToUpdate = new List<UserRoutine>();
            List<UserRoutine> userRoutinesToAdd = new List<UserRoutine>();

            foreach (var routine in routines)
            {
                var existingRoutine = existingUserRoutines.FirstOrDefault(ur => ur.RoutineId == routine.SkincareRoutineId);
    
                if (existingRoutine != null && existingRoutine.Status == ObjectStatus.Suitable.ToString())
                {
                    // Nếu đã tồn tại, cập nhật thông tin
                    existingRoutine.Status = ObjectStatus.Suitable.ToString();
                    existingRoutine.ProgressNotes = "Updated routine for your skin";
                    existingRoutine.UpdatedDate = DateTime.Now;

                    userRoutinesToUpdate.Add(existingRoutine);
                }
                else if (existingRoutine != null && existingRoutine.Status == ObjectStatus.Active.ToString())
                {
                    // Nếu đã tồn tại đang sử dụng, cập nhật thông tin
                    continue;
                }
                else
                {
                    // Nếu chưa tồn tại, thêm mới
                    var newRoutine = new UserRoutine()
                    {
                        UserId = userId,
                        RoutineId = routine.SkincareRoutineId,
                        Status = ObjectStatus.Suitable.ToString(),
                        ProgressNotes = "Suitable for your skin",
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now.AddMonths(1),
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };
                    userRoutinesToAdd.Add(newRoutine);
                }
            }

// Cập nhật và thêm mới dữ liệu
            if (userRoutinesToUpdate.Any())
            {
              await _unitOfWorks.UserRoutineRepository.UpdateRangeAsync(userRoutinesToUpdate);
            }

            if (userRoutinesToAdd.Any())
            {
                await _unitOfWorks.UserRoutineRepository.AddRangeAsync(userRoutinesToAdd);
            }

// Lưu thay đổi vào database
            await _unitOfWorks.UserRoutineRepository.Commit();


            
            var result = new ApiSkinAnalyzeResponse()
            {
                skinhealth = apiResult.result,
                routines = _mapper.Map<List<SkincareRoutineModel>>(routines)
            };

            // Map routines to DTOs
            return new SkinAnalyzeResponse()
            {
                message = "Analyze skin successfully",
                data = result
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogInformation("AI_SKIN: " + ex.Message);
           throw new BadRequestException(ex.Message);
        }
    }
    
    
    public async Task<SkinAnalyzeResponse> AnalyzeSkinFromFormAsync(SkinHealthFormRequest request, int userId)
    {
        try
        {
            // Map API response to SkinHealth entity
            var skinHealth = new SkinHealth
            {
                UserId = userId,
                SkinColor = GetApiResponseValue(request.skin_color),
                Acne = GetApiResponseValue(request.acne ?? null),
                SkinToneIta = GetApiResponseValue(request.skintone_ita),
                SkinTone = GetApiResponseValue(request.skin_tone),
                SkinHueHa = GetApiResponseValue(request.skin_hue_ha),
                SkinAge = GetApiResponseValue(request.skin_age),
                SkinType = GetApiResponseValue(request.skin_type),
                LeftEyelids = GetApiResponseValue(request.left_eyelids),
                RightEyelids = GetApiResponseValue(request.right_eyelids),
                EyePouch = GetApiResponseValue(request.eye_pouch),
                EyePouchSeverity = GetApiResponseValue(request.eye_pouch_severity),
                DarkCircle = GetApiResponseValue(request.dark_circle),
                ForeheadWrinkle = GetApiResponseValue(request.forehead_wrinkle),
                CrowsFeet = GetApiResponseValue(request.crows_feet),
                GlabellaWrinkle = GetApiResponseValue(request.glabella_wrinkle),
                NasolabialFold = GetApiResponseValue(request.nasolabial_fold),
                NasolabialFoldSeverity = GetApiResponseValue(request.nasolabial_fold_severity),
                PoresForehead = GetApiResponseValue(request.pores_forehead),
                PoresLeftCheek = GetApiResponseValue(request.pores_left_cheek),
                PoresRightCheek = GetApiResponseValue(request.pores_right_cheek),
                PoresJaw = GetApiResponseValue(request.pores_jaw),
                BlackHead = GetApiResponseValue(request.blackhead),
                Rectangle = GetApiResponseValue(request.rectangle),
                Mole = GetApiResponseValue(request.mole ?? null),
                ClosedComedones = GetApiResponseValue(request.closed_comedones ?? null),
                SkinSpot = GetApiResponseValue(request.skin_spot ?? null),
                FaceMaps = GetApiResponseValue(request.face_maps),
                Sensitivity = GetApiResponseValue(request.sensitivity),
                SensitivityArea = GetApiResponseValue(request.sensitivity_area),
                SensitivityIntensity = GetApiResponseValue(request.sensitivity_intensity),
                EyeFineLines = GetApiResponseValue(request.eye_finelines),
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            // Save SkinHealth data to the database
            await _unitOfWorks.SkinHealthRepository.AddAsync(skinHealth);
            await _unitOfWorks.SkinHealthRepository.Commit();

            // Process skin concerns
            var skinConcerns = GetSkinConcernsFromForm(request);
            var routines = await GetSkincareRoutinesAsync(skinConcerns);

            List<UserRoutine> userRoutines = new List<UserRoutine>();
            foreach (var routine in routines)
            {
                var existingRoutine = await _unitOfWorks.UserRoutineRepository
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoutineId == routine.SkincareRoutineId);

                if (existingRoutine != null && existingRoutine.Status == ObjectStatus.Suitable.ToString())
                {
                    // Nếu đã tồn tại, cập nhật thông tin
                    existingRoutine.Status = ObjectStatus.Suitable.ToString();
                    existingRoutine.ProgressNotes = "Updated routine for your skin";
                    existingRoutine.UpdatedDate = DateTime.Now;
                    _unitOfWorks.UserRoutineRepository.Update(existingRoutine);
                    await _unitOfWorks.UserRoutineRepository.Commit();
                }
                else if (existingRoutine != null && existingRoutine.Status == ObjectStatus.Active.ToString())
                {
                    // Nếu đã tồn tại đang sử dụng, cập nhật thông tin
                    continue;
                }
                else
                {
                    // Nếu chưa tồn tại, thêm mới
                    var newRoutine = new UserRoutine()
                    {
                        UserId = userId,
                        RoutineId = routine.SkincareRoutineId,
                        Status = ObjectStatus.Suitable.ToString(),
                        ProgressNotes = "Suitable for your skin",
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now.AddMonths(1),
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };
                    userRoutines.Add(newRoutine);
                }
            }

            // Chỉ thêm nếu có dữ liệu mới
            if (userRoutines.Any())
            {
                await _unitOfWorks.UserRoutineRepository.AddRangeAsync(userRoutines);
            }

            // Lưu thay đổi
            await _unitOfWorks.UserRoutineRepository.Commit();

            
            var result = new ApiSkinAnalyzeResponse()
            {
                skinhealth = request,
                routines = _mapper.Map<List<SkincareRoutineModel>>(routines)
            };

            // Map routines to DTOs
            return new SkinAnalyzeResponse()
            {
                message = "Analyze skin successfully",
                data = result
            };
        }
        catch (HttpRequestException ex)
        {
           throw new BadRequestException(ex.Message);
        }
    }

    private static string GetApiResponseValue(dynamic apiResponse)
    {
        return JsonConvert.SerializeObject(apiResponse);
    }



    private List<(string Concern, double Confidence)> GetSkinConcerns(dynamic apiResult)
    {
        return new List<(string Concern, double Confidence)>
        {
            ("Oily Skin", (double)(apiResult.result.skin_type?.details[0]?.confidence ?? 0)),
            ("Dry Skin", (double)(apiResult.result.skin_type?.details[1]?.confidence ?? 0)),
            ("Neutral Skin", (double)(apiResult.result.skin_type?.details[2]?.confidence ?? 0)),
            ("Combination Skin", (double)(apiResult.result.skin_type?.details[3]?.confidence ?? 0)),
            ("Blackheads", (double)(apiResult.result.blackhead?.confidence ?? 0)),
            ("Acne", (double)(apiResult.result.acne?.confidence ?? 0)),
            ("Dark Circles", (double)(apiResult.result.dark_circle?.confidence ?? 0)),
            ("Closed Comedones", (double)(apiResult.result.closed_comedones?.confidence ?? 0)),
            ("Glabella Wrinkles", (double)(apiResult.result.glabella_wrinkle?.confidence ?? 0))
        };
    }
    
    private List<(string Concern, double Confidence)> GetSkinConcernsFromForm(dynamic apiResult)
    {
        return new List<(string Concern, double Confidence)>
        {
            ("Da dầu", (double)(apiResult.skin_type?.details[0]?.confidence ?? 0)),
            ("Da khô", (double)(apiResult.skin_type?.details[1]?.confidence ?? 0)),
            ("Da trung tính", (double)(apiResult.skin_type?.details[2]?.confidence ?? 0)),
            ("Da hỗn hợp", (double)(apiResult.skin_type?.details[3]?.confidence ?? 0)),
            ("Mụn đầu đen", (double)(apiResult.blackhead?.confidence ?? 0)),
            ("Mụn trứng cá", (double)(apiResult.acne != null ? apiResult.acne.confidence ?? 0 : 0)),
            ("Quầng thâm mắt", (double)(apiResult.dark_circle?.confidence ?? 0)),
            ("Mụn có nhân đóng", (double)(apiResult.closed_comedones?.confidence ?? 0)),
            ("Nếp nhăn Glabella", (double)(apiResult.glabella_wrinkle?.confidence ?? 0))
        };
    }

    private async Task<List<SkincareRoutine>> GetSkincareRoutinesAsync(List<(string Concern, double Confidence)> skinConcerns)
    {
        var prioritizedConcerns = skinConcerns
            .Where(c => c.Confidence > 0)
            .OrderByDescending(c => c.Confidence)
            .ToList();

        var routines = new List<SkincareRoutine>();
        foreach (var concern in prioritizedConcerns)
        {
            var matchingRoutines = await _unitOfWorks.SkincareRoutineRepository
                .FindByCondition(r => r.TargetSkinTypes.Contains(concern.Concern))
                .ToListAsync();
            routines.AddRange(matchingRoutines);
        }

        return routines
            .GroupBy(r => r.SkincareRoutineId)
            .Select(g => g.First())
            .OrderByDescending(r => prioritizedConcerns.FirstOrDefault(c => r.TargetSkinTypes!.Contains(c.Concern)).Confidence)
            .ToList();
    }

    
    public async Task<List<SkinHealthImage>> GetSkinHealthImages(int userId)
    {
        var skinHealthImages = await _unitOfWorks.SkinHealthImageRepository
            .FindByCondition(shi => shi.SkinHealth.UserId == userId)
            .Include(x => x.SkinHealth)
            .ToListAsync();

        return skinHealthImages;
    }


}
