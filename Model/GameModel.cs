
using QFramework;
using SurvivalGame.Model;

namespace SurvivalGame
{
    public interface IGameModel:QFIModel
    {
        
        public BindableProperty<int> SkyBlazeSphere { get; set; }
        public SurvivorSystemData SurvivorData { get; set; }
      
    }
    public class GameModel: AbstractModel, IGameModel
    {
        //public BindableProperty<int> SkyBlazeSphere { get; set; }
        public SurvivorSystemData SurvivorData { get; set; }
      
        protected override void OnInit()
        {
            
            SkyBlazeSphere = new BindableProperty<int>(0);
            SurvivorData = new SurvivorSystemData();
            
        }

        public BindableProperty<int> SkyBlazeSphere { get; set; }
    }
}