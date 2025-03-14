using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("SkinHealth")]
public class SkinHealth
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SkinHealthId { get; set; }
    
    [ForeignKey("UserSkinHealth")]
    public int UserId { get; set; }
    public virtual User User { get; set; }
    
    public string? SkinColor { get; set; }
    public string? SkinToneIta { get; set; } // Returns skin color classification information based on the ITA (Individual Typology Angle) standard.
    public string? SkinTone { get; set; }
    public string? SkinHueHa { get; set; } // Returns skin tone classification information based on HA (Hue Angle).
    public string? SkinAge { get; set; }
    public string? SkinType { get; set; }
    public string? LeftEyelids { get; set; }
    public string? RightEyelids { get; set; }
    public string? EyePouch { get; set; }
    public string? EyePouchSeverity { get; set; }
    public string? DarkCircle { get; set; }
    public string? ForeheadWrinkle { get; set; }
    public string? CrowsFeet { get; set; }
    public string? EyeFineLines { get; set; }
    public string? GlabellaWrinkle { get; set; }
    public string? NasolabialFold { get; set; }
    public string? NasolabialFoldSeverity { get; set; }
    public string? PoresForehead { get; set; }
    public string? PoresLeftCheek { get; set; }
    public string? PoresRightCheek { get; set; }
    public string? PoresJaw { get; set; }
    public string? BlackHead { get; set; }
    public string? Rectangle { get; set; }
    public string? Mole { get; set; }
    public string? ClosedComedones { get; set; }
    public string? SkinSpot { get; set; }
    public string? FaceMaps { get; set; }
    public string? Sensitivity { get; set; }
    public string? SensitivityArea { get; set; }
    public string? SensitivityIntensity { get; set; }
    
    public string? Acne { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}