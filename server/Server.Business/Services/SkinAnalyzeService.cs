﻿using System.Net.Http.Headers;
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
using Newtonsoft.Json.Linq;

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
            // Lấy phần "result" ra dưới dạng JSON string
            var resultJson = JObject.Parse(responseData)["result"]?.ToString();

            // Deserialize phần "result" thành SkinHealthFormRequest
            var apiResult = JsonConvert.DeserializeObject<SkinHealthFormRequest>(resultJson);

            // Map API response to SkinHealth entity
            var skinHealth = new SkinHealth
            {
                UserId = userId,
                Acne = GetApiResponseValue(apiResult?.acne),
                SkinColor = GetApiResponseValue(apiResult?.skin_color),
                SkinToneIta = GetApiResponseValue(apiResult?.skintone_ita),
                SkinTone = GetApiResponseValue(apiResult?.skin_tone),
                SkinHueHa = GetApiResponseValue(apiResult?.skin_hue_ha),
                SkinAge = GetApiResponseValue(apiResult?.skin_age),
                SkinType = GetApiResponseValue(apiResult?.skin_type),
                LeftEyelids = GetApiResponseValue(apiResult?.left_eyelids),
                RightEyelids = GetApiResponseValue(apiResult?.right_eyelids),
                EyePouch = GetApiResponseValue(apiResult?.eye_pouch),
                EyePouchSeverity = GetApiResponseValue(apiResult?.eye_pouch_severity),
                DarkCircle = GetApiResponseValue(apiResult?.dark_circle),
                ForeheadWrinkle = GetApiResponseValue(apiResult?.forehead_wrinkle),
                CrowsFeet = GetApiResponseValue(apiResult?.crows_feet),
                GlabellaWrinkle = GetApiResponseValue(apiResult?.glabella_wrinkle),
                NasolabialFold = GetApiResponseValue(apiResult?.nasolabial_fold),
                NasolabialFoldSeverity = GetApiResponseValue(apiResult?.nasolabial_fold_severity),
                PoresForehead = GetApiResponseValue(apiResult?.pores_forehead),
                PoresLeftCheek = GetApiResponseValue(apiResult?.pores_left_cheek),
                PoresRightCheek = GetApiResponseValue(apiResult?.pores_right_cheek),
                PoresJaw = GetApiResponseValue(apiResult?.pores_jaw),
                BlackHead = GetApiResponseValue(apiResult?.blackhead),
                Rectangle = GetApiResponseValue(apiResult?.rectangle),
                Mole = GetApiResponseValue(apiResult?.mole),
                ClosedComedones = GetApiResponseValue(apiResult?.closed_comedones),
                SkinSpot = GetApiResponseValue(apiResult?.skin_spot),
                FaceMaps = GetApiResponseValue(apiResult?.face_maps),
                Sensitivity = GetApiResponseValue(apiResult?.sensitivity),
                SensitivityArea = GetApiResponseValue(apiResult?.sensitivity_area),
                SensitivityIntensity = GetApiResponseValue(apiResult?.sensitivity_intensity),
                EyeFineLines = GetApiResponseValue(apiResult?.eye_finelines),
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
            var skinConcerns = await GetSkinConcernsAsync(apiResult!);

            // Retrieve skincare routines based on concerns
            var routines = await GetSkincareRoutinesAsync(skinConcerns);

            // Lấy danh sách các routine hiện tại của user
            var existingUserRoutines = await _unitOfWorks.UserRoutineRepository
                .FindByCondition(ur => ur.UserId == userId).ToListAsync();

            List<UserRoutine> userRoutinesToUpdate = new List<UserRoutine>();
            List<UserRoutine> userRoutinesToAdd = new List<UserRoutine>();
            List<UserRoutine> userRoutinesToDelete = new List<UserRoutine>();

            var newRoutineIds = routines.Select(r => r.SkincareRoutineId).ToHashSet();

            foreach (var routine in routines)
            {
                var existingRoutinesForThisId = existingUserRoutines
                    .Where(ur => ur.RoutineId == routine.SkincareRoutineId)
                    .ToList();

                var activeRoutine = existingRoutinesForThisId
                    .FirstOrDefault(r => r.Status == ObjectStatus.Active.ToString());

                if (activeRoutine != null)
                {
                    // Đang sử dụng thì giữ nguyên
                    continue;
                }
                
                var completedRoutine = existingRoutinesForThisId
                    .FirstOrDefault(r => r.Status == ObjectStatus.Completed.ToString());

                if (completedRoutine != null)
                {
                    var suitabledRoutine = existingRoutinesForThisId
                        .FirstOrDefault(r => r.Status == ObjectStatus.Suitable.ToString());
                    if (suitabledRoutine != null)
                    {
                        // Nếu đã có routine suitable thì không cần tạo mới
                        continue;
                    }
                    // Đã hoàn thành thì tạo bản mới
                    var newRoutine = new UserRoutine
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
                    continue;
                }
                
                var suitableRoutine = existingRoutinesForThisId
                    .FirstOrDefault(r => r.Status == ObjectStatus.Suitable.ToString());

                if (suitableRoutine != null)
                {
                    suitableRoutine.ProgressNotes = "Updated routine for your skin";
                    suitableRoutine.UpdatedDate = DateTime.Now;
                    userRoutinesToUpdate.Add(suitableRoutine);
                }
                else
                {
                    // Không có thì tạo mới
                    var newRoutine = new UserRoutine
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

            // Xử lý routine cũ không còn trong danh sách mới và KHÔNG phải là Active
            foreach (var existingRoutine in existingUserRoutines)
            {
                if (!newRoutineIds.Contains(existingRoutine.RoutineId) &&
                    existingRoutine.Status != ObjectStatus.Active.ToString() &&
                    existingRoutine.Status != ObjectStatus.Completed.ToString())
                {
                    existingRoutine.Status = ObjectStatus.InActive.ToString();
                    userRoutinesToDelete.Add(existingRoutine);
                }
            }

            // Cập nhật, thêm mới và xóa
            if (userRoutinesToUpdate.Any())
            {
                await _unitOfWorks.UserRoutineRepository.UpdateRangeAsync(userRoutinesToUpdate);
            }

            if (userRoutinesToAdd.Any())
            {
                await _unitOfWorks.UserRoutineRepository.AddRangeAsync(userRoutinesToAdd);
            }

            if (userRoutinesToDelete.Any())
            {
                await _unitOfWorks.UserRoutineRepository.UpdateRangeAsync(userRoutinesToDelete);
            }

            // Lưu thay đổi
            await _unitOfWorks.UserRoutineRepository.Commit();


            var result = new ApiSkinAnalyzeResponse()
            {
                skinhealth = apiResult ?? new SkinHealthFormRequest(),
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
            var skinConcerns = await GetSkinConcernsAsync(request);

            // Retrieve skincare routines based on concerns
            var routines = await GetSkincareRoutinesAsync(skinConcerns);

            // Lấy danh sách các routine hiện tại của user
            var existingUserRoutines = await _unitOfWorks.UserRoutineRepository
                .FindByCondition(ur => ur.UserId == userId).ToListAsync();

            List<UserRoutine> userRoutinesToUpdate = new List<UserRoutine>();
            List<UserRoutine> userRoutinesToAdd = new List<UserRoutine>();
            List<UserRoutine> userRoutinesToDelete = new List<UserRoutine>();

            var newRoutineIds = routines.Select(r => r.SkincareRoutineId).ToHashSet();

            foreach (var routine in routines)
            {
                var existingRoutinesForThisId = existingUserRoutines
                    .Where(ur => ur.RoutineId == routine.SkincareRoutineId)
                    .ToList();

                var activeRoutine = existingRoutinesForThisId
                    .FirstOrDefault(r => r.Status == ObjectStatus.Active.ToString());

                if (activeRoutine != null)
                {
                    // Đang sử dụng thì giữ nguyên
                    continue;
                }
                
                var completedRoutine = existingRoutinesForThisId
                    .FirstOrDefault(r => r.Status == ObjectStatus.Completed.ToString());

                if (completedRoutine != null)
                {
                    var suitabledRoutine = existingRoutinesForThisId
                        .FirstOrDefault(r => r.Status == ObjectStatus.Suitable.ToString());
                    if (suitabledRoutine != null)
                    {
                        // Nếu đã có routine suitable thì không cần tạo mới
                        continue;
                    }
                    // Đã hoàn thành thì tạo bản mới
                    var newRoutine = new UserRoutine
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
                    continue;
                }
                
                var suitableRoutine = existingRoutinesForThisId
                    .FirstOrDefault(r => r.Status == ObjectStatus.Suitable.ToString());

                if (suitableRoutine != null)
                {
                    suitableRoutine.ProgressNotes = "Updated routine for your skin";
                    suitableRoutine.UpdatedDate = DateTime.Now;
                    userRoutinesToUpdate.Add(suitableRoutine);
                }
                else
                {
                    // Không có thì tạo mới
                    var newRoutine = new UserRoutine
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

            // Xử lý routine cũ không còn trong danh sách mới và KHÔNG phải là Active
            foreach (var existingRoutine in existingUserRoutines)
            {
                if (!newRoutineIds.Contains(existingRoutine.RoutineId) &&
                    existingRoutine.Status != ObjectStatus.Active.ToString() &&
                    existingRoutine.Status != ObjectStatus.Completed.ToString())
                {
                    existingRoutine.Status = ObjectStatus.InActive.ToString();
                    userRoutinesToDelete.Add(existingRoutine);
                }
            }

            // Cập nhật, thêm mới và xóa
            if (userRoutinesToUpdate.Any())
            {
                await _unitOfWorks.UserRoutineRepository.UpdateRangeAsync(userRoutinesToUpdate);
            }

            if (userRoutinesToAdd.Any())
            {
                await _unitOfWorks.UserRoutineRepository.AddRangeAsync(userRoutinesToAdd);
            }

            if (userRoutinesToDelete.Any())
            {
                await _unitOfWorks.UserRoutineRepository.UpdateRangeAsync(userRoutinesToDelete);
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


    private async Task<List<(string Concern, double Confidence)>> GetSkinConcernsAsync(dynamic apiResult)
    {
        var skinConcerns = await _unitOfWorks.SkinConcernRepository.GetAll().ToListAsync();
        var result = new List<(string Concern, double Confidence)>();

        // Nếu có skin_type hợp lệ (bao gồm cả = 0)
        if (apiResult.skin_type?.skin_type != null)
        {
            int index = apiResult.skin_type.skin_type;
            var detail = apiResult.skin_type.details?[index];
            double confidence = double.Parse(detail?.confidence?.ToString()) ?? 0;

            // Tìm đúng concern có Code = "skin_type_{index}"
            var matchedConcern = skinConcerns.FirstOrDefault(sc => sc.Code == $"skin_type_{index}");
            if (matchedConcern != null && confidence > 0)
            {
                result.Add((matchedConcern.Name, confidence));
            }
        }

        foreach (var skinConcern in skinConcerns)
        {
            double confidence = 0;

            // Bỏ qua các concern bắt đầu bằng "skin_type_"
            if (skinConcern.Code.StartsWith("skin_type_"))
            {
                continue;
            }

            // Xử lý các concern khác như acne, wrinkle...
            confidence = ExtractConfidence(apiResult, skinConcern.Code);

            if (confidence > 0)
            {
                result.Add((skinConcern.Name, confidence));
            }
        }

        return result;
    }


    private async Task<List<SkincareRoutine>> GetSkincareRoutinesAsync(
        List<(string Concern, double Confidence)> skinConcerns)
    {
        // Lọc các concern có độ tin cậy > 0
        var prioritizedConcerns = skinConcerns
            .Where(c => c.Confidence > 0)
            .OrderByDescending(c => c.Confidence)
            .ToList();

        var routines = new List<SkincareRoutine>();

        foreach (var concern in prioritizedConcerns)
        {
            // Tìm các SkinCareRoutine có concern liên quan
            var matchingRoutines = await _unitOfWorks.SkinCareConcernRepository
                .FindByCondition(sc => sc.SkinConcern.Name.ToLower().Contains(concern.Concern.ToLower()))
                .Select(sc => sc.SkincareRoutine)
                .ToListAsync();

            routines.AddRange(matchingRoutines);
        }

        // Trả về danh sách routine duy nhất theo độ ưu tiên của concern
        return routines
            .GroupBy(r => r.SkincareRoutineId)
            .Select(g => g.First())
            .OrderByDescending(r => prioritizedConcerns
                .FirstOrDefault(c => r.TargetSkinTypes!.ToLower().Contains(c.Concern.ToLower()))
                .Confidence) // map skin_concern.Name with skin_routine.TargetSkinTypes
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


    private double ExtractConfidence(dynamic apiResult, string key)
    {
        try
        {
            var data = apiResult?.GetType().GetProperty(key)?.GetValue(apiResult, null);
            if (data == null) return 0;

            // Trường hợp có property "confidence"
            if (data.confidence != null && data.confidence != 0 && data.value != null && data.value != 0)
            {
                double confidence = data.confidence ?? 0;
                if (confidence != 0)
                {
                    return Convert.ToDouble(data.value);
                }
            }

            // Trường hợp có property "rectangle" (đếm số lượng)
            if (data.rectangle != null)
            {
                var rectangles = data.rectangle as IEnumerable<dynamic>;
                return rectangles != null ? rectangles.Count() : 0;
            }
        }
        catch
        {
            // Ignore and return 0 if anything goes wrong
        }

        return 0;
    }
}