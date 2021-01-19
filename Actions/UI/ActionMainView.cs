using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

namespace Actions.UI
{
    [ViewDefinition("Actions.Views.main-view.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\main-view.bsml")]
    internal class ActionMainView : BSMLAutomaticViewController
    {
    }
}