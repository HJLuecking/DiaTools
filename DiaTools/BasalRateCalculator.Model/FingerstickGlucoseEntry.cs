namespace Diary.Model;

public class FingerstickGlucoseEntry
{
    public DateTime Time { get; set; }

    public double MMolPerLitre
    {
        get;
        set
        {
            field = value;
            MgPerLitre = field * ModelConstants.MMolToMgDl;
        }
    }


    public double MgPerLitre { get; set; }
}