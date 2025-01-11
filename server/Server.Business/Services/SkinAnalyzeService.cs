using System.Net.Http.Headers;
using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Server.Business.Commons.Response;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Business.Ultils;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services;

public class SkinAnalyzeService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly IMapper _mapper;
    private readonly AISkinSetting _aiSkinSetting;
    private static readonly HttpClient HttpClient = new HttpClient();

    public SkinAnalyzeService(UnitOfWorks unitOfWorks, IMapper mapper, IOptions<AISkinSetting> aiSkinSetting)
    {
        _unitOfWorks = unitOfWorks;
        _mapper = mapper;
        _aiSkinSetting = aiSkinSetting.Value;
    }

    public async Task<SkinAnalyzeResponse> AnalyzeSkinAsync(IFormFile file, int userId)
    {
        if (file == null || file.Length == 0)
        {
            throw new BadRequestException("File cannot be null or empty.");
        }

        using var content = new MultipartFormDataContent();
        using var fileStream = file.OpenReadStream();
        var fileContent = new StreamContent(fileStream)
        {
            Headers = { ContentType = MediaTypeHeaderValue.Parse(file.ContentType) }
        };
        content.Add(fileContent, "image", file.FileName);

        HttpClient.DefaultRequestHeaders.Add("ailabapi-api-key", _aiSkinSetting.ApiKey);

        try
        {
            var response = await HttpClient.PostAsync(_aiSkinSetting.Url, content);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStringAsync();
            var apiResult = JsonConvert.DeserializeObject<dynamic>(responseData);


            var test = apiResult.result.skin_type;
            // Map API response to SkinHealth entity
            var skinHealth = new SkinHealth
            {
                UserId = userId,
                Ance = GetApiResponseValue(apiResult.result.acne),
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

            // Process skin concerns
            var skinConcerns = GetSkinConcerns(apiResult);

            // Retrieve skincare routines based on concerns
            var routines = await GetSkincareRoutinesAsync(skinConcerns);

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
            .OrderByDescending(r => prioritizedConcerns.FirstOrDefault(c => r.TargetSkinTypes.Contains(c.Concern)).Confidence)
            .ToList();
    }





}
