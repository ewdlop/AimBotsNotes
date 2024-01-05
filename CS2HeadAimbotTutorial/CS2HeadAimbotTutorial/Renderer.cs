using ClickableTransparentOverlay;
using ImGuiNET;

namespace CS2HeadAimbotTutorial
{
    public class Renderer : Overlay
    {
        public bool aimbot = true;
        public bool aimOnTeam = false;
        public bool wallHack = true;

        protected override void Render()
        {

            ImGui.Begin("menu");
            ImGui.Checkbox("aimbot", ref aimbot);
            ImGui.Checkbox("aim on teammates", ref aimOnTeam);
            ImGui.Checkbox("see through wall", ref wallHack);
        }
    }
}
