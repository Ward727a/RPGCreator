using Gum.Wireframe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameGum.Forms.Controls.Primitives;
using RenderingLibrary.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.MonoGame.Editor.Components.UI.Buttons
{
    public class ButtonEditortt : ButtonBase
    {
        public const string ButtonCategoryState = "ButtonCategoryState";
        private GraphicalUiElement textComponent;
        private Text coreTextObject;

        public event EventHandler Dragged;

        public string Text
        {
            get
            {
                ReportMissingTextInstance();
                return coreTextObject.RawText;
            }
            set
            {
                ReportMissingTextInstance();
                textComponent?.SetProperty("Text", value);
            }
        }

        public ButtonEditortt() { }
        public ButtonEditortt(InteractiveGue visual)
            : base(visual) { }

        public override bool IsEnabled { 
            get => base.IsEnabled;
            set {
                base.IsEnabled = value;
                UpdateState();
            } 
        }


        protected override void ReactToVisualChanged()
        {
            textComponent = base.Visual.GetGraphicalUiElementByName("TextInstance");
            coreTextObject = textComponent?.RenderableComponent as Text;

            base.Visual.Dragging += Visual_Dragging;

            base.ReactToVisualChanged();
        }

        private void Visual_Dragging(object? sender, EventArgs e)
        {
            UpdateState();
            OnDrag();
            this.Dragged?.Invoke(this, e);
        }

        protected virtual void OnDrag()
        {
            Point newPos = Mouse.GetState().Position;
            this.X = newPos.X;
            this.Y = newPos.Y;
        }

        public override void UpdateState()
        {
            string desiredState = GetDesiredState();
            base.Visual.SetProperty("ButtonCategoryState", desiredState);
        }

        private void ReportMissingTextInstance()
        {
            if(textComponent == null)
            {
                throw new Exception($"This button was created with a Gum component ({base.Visual?.ElementSave}) " + "that does not have an instance called 'TextInstance'. A 'TextInstance' instance must be added to modify the button's Text property.");
            }
        }

    }
}
