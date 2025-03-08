using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using RPGCreator.core.helpers;
using RPGCreator.core.types.Math.Transform;
using RPGCreator.core.UI.components;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.UI.containers
{
    class SimpleContainer : BaseUI
    {
        protected RenderTarget2D render;
        protected List<BaseUI> Childs = [];
        protected Dictionary<string, int> ChildsIndexes = [];

        public event EventHandler<BaseUI> OnAddChildren;
        public event EventHandler<BaseUI> OnRemoveChildren;

        public SimpleContainer(Scale scale)
        {
            CallPreInit();
            _Scale = scale;
            Init();
            CallPostInit();
        }

        public virtual SimpleContainer Init()
        {

            render = new RenderTarget2D(Game1.GetGraphicDevice(), (int)GetScale().X, (int)GetScale().Y);
            return this;
        }

        public virtual bool AddChild(BaseUI child)
        {
            if(ChildsIndexes.TryGetValue(child.ObjectName, out _))
            {
                Log.Logger.Error($"Container already have child with name {child.ObjectName}.");
                return false;
            }
            child.parent = this;
            if (Childs.Count > 0)
            {
                child.SetPosition(new(child.GetRelativePosition().X, child.GetRelativePosition().Y + Childs.Last().GetScale().Y));
                Log.Logger.Verbose($"New position: {new Position(child.GetRelativePosition().X, child.GetRelativePosition().Y + Childs.Last().GetScale().Y)}");
            }
            ChildsIndexes.Add(child.ObjectName, Childs.Count);
            Childs.Add(child);
            OnAddChildren(this, child);
            return true;
        }

        public virtual bool RemoveChild(BaseUI child)
        {
            if (!ChildsIndexes.Remove(child.ObjectName, out int index))
            {
                Log.Logger.Error($"Container doesn't have child with name {child.ObjectName}.");
                return false;
            }
            Childs[index].parent = null;
            Childs.RemoveAt(index);
            OnRemoveChildren(this, child);

            return true;
        }

        public virtual bool RemoveChild(string key)
        {
            if (!ChildsIndexes.Remove(key, out int index))
            {
                Log.Logger.Error($"Container doesn't have child with key {key}.");
                return false;
            }
            Childs[index].parent = null;
            OnRemoveChildren(this, Childs[index]);
            Childs.RemoveAt(index);

            return true;
        }

        
        public virtual T GetChild<T>(string key) where T : BaseUI
        {
            if(!ChildsIndexes.TryGetValue(key, out int index))
            {
                Log.Logger.Error($"Container doesn't have child with name {key}.");
                return default;
            }

            return (T)Childs[index];
        }

        public virtual BaseUI GetChild(string key)
        {
            return GetChild<BaseUI>(key);
        }


        public override void Draw(SpriteBatchExtended _sb)
        {
            render.BeginDraw(Game1.GetGraphicDevice(), Color.Red);

            DrawContents(_sb);
            FinalizeDrawContainer(_sb);
        }

        public override void Update(GameTime gameTime)
        {
            foreach(BaseUI child in Childs)
            {
                child._Update(gameTime);
            }
        }

        public virtual void DrawContents(SpriteBatchExtended _sb)
        {
            _sb.Begin();

            foreach (BaseUI child in Childs)
            {
                child._Draw(_sb);
            }

            _sb.End();
        }

        public virtual void FinalizeDrawContainer(SpriteBatchExtended _sb)
        {
            Game1.GetGraphicDevice().SetRenderTarget(null);
            _sb.Begin();
            _sb.Draw(render, new Vector2(GetAbsolutePosition().X, GetAbsolutePosition().Y), Color.White);
            _sb.End();
        }
    }
}
