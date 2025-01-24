namespace Server.Business.Commons.Request;

public class SkinHealthFormRequest
{
    public dynamic? skin_color { get; set; }
    public dynamic? skintone_ita { get; set; } // Returns skin color classification information based on the ITA (Individual Typology Angle) standard.
    public dynamic? skin_tone { get; set; }
    public dynamic? skin_hue_ha { get; set; } // Returns skin tone classification information based on HA (Hue Angle).
    public dynamic? skin_age { get; set; }
    public dynamic? skin_type { get; set; }
    public dynamic? left_eyelids { get; set; }
    public dynamic? right_eyelids { get; set; }
    public dynamic? eye_pouch { get; set; }
    public dynamic? eye_pouch_severity { get; set; }
    public dynamic? dark_circle { get; set; }
    public dynamic? forehead_wrinkle { get; set; }
    public dynamic? crows_feet { get; set; }
    public dynamic? eye_finelines { get; set; }
    public dynamic? glabella_wrinkle { get; set; }
    public dynamic? nasolabial_fold { get; set; }
    public dynamic? nasolabial_fold_severity { get; set; }
    public dynamic? pores_forehead { get; set; }
    public dynamic? pores_left_cheek { get; set; }
    public dynamic? pores_right_cheek { get; set; }
    public dynamic? pores_jaw { get; set; }
    public dynamic? blackhead { get; set; }
    public dynamic? rectangle { get; set; }
    public dynamic? mole { get; set; }
    public dynamic? closed_comedones { get; set; }
    public dynamic? skin_spot { get; set; }
    public dynamic? face_maps { get; set; }
    public dynamic? sensitivity { get; set; }
    public dynamic? sensitivity_area { get; set; }
    public dynamic? sensitivity_intensity { get; set; }
    public dynamic? ance { get; set; }
}
