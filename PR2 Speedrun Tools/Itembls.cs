using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR2_Speedrun_Tools
{
    class Itembls
    {
        protected LocalCharacter racer;
        protected bool space = false;
        protected bool reloading = false;
        private uint reloadListener;
        private bool available = false;

        public Itembls(LocalCharacter param1)
        {
            this.racer = param1;
            this.setReloadTime(10);
            this.setUses(1);
        }

        public void setSpace(bool param1)
        {
            this.space = param1;
            if (!this.space) {
                this.available = true;
            }
            var _loc2_ = racer.ItemUses;//class_33.getNumber("uses");
            if (this.space && _loc2_ > 0 && !this.reloading && this.available) {
                this.useItem();
            }
        }

        protected void setUses(int param1)
        {
            this.racer.ItemUses = param1;//class_33.setNumber("uses", param1);
            //this.racer.setAmmo(param1);
        }

        protected void setReloadTime(int param1)
        {
            this.racer.ItemTime = param1;//class_33.setNumber("reloadTime", param1);
        }

        public void useItem()
        {
            this.racer.ItemUses--;
            //this.racer.setAmmo(this.racer.ItemUses);
            if (this.racer.ItemUses <= 0) {
                //this.racer.setItem(0);
            }
            else {
                this.reloading = true;
                //this.reloadListener = setTimeout(this.reloadingOnComplete, class_33.getNumber("reloadTime"));
            }
        }

        private void reloadingOnComplete()
        {
            this.reloading = false;
        }

        protected Point method_37()
        {
            //var _loc1_:Point = new Point(this.racer.curWeapon.x, this.racer.curWeapon.y);
            //_loc1_ = this.racer.curWeapon.parent.localToGlobal(_loc1_);
            //return class_87.var_276.globalToLocal(_loc1_);
            return default(Point);
        }


    }
}
