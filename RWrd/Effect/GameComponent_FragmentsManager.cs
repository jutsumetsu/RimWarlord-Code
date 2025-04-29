using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Effect
{
    public class GameComponent_FragmentsManager : GameComponent
    {
        public GameComponent_FragmentsManager(Game game) 
        {
        }
        public override void FinalizeInit()
        {
            base.FinalizeInit();
            this.InitializeFragments();
        }
        private void InitializeFragments()
        {
            try
            {
                Worker.Start();
            }
            catch (Exception arg)
            {
                Log.Error(string.Format("Exception thrown while initializing Fragments. Worker = \"{0}\" Exception = {1}", this.Worker, arg));
            }


        }
        PowerfulPersonFragments Worker = new PowerfulPersonFragments();
    }
}
