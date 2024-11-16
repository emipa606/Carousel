using Verse;

namespace Carousel;

public class Settings : ModSettings
{
    public bool disableCompass;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref disableCompass, "disableCompass");
    }
}