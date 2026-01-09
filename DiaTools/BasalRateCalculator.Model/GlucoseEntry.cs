namespace Diary.Model;


public class GlucoseEntry
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