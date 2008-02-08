using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace DetailLogPlugin
{
    public class ReadOnlyComboBox : ComboBox
    {
        #region�@���̃N���X�̃t�B�[���h �����o

        private System.ComponentModel.IContainer components;
        private System.Drawing.Color oldBackColor;
        private bool keyPressHandled;

        #endregion

        #region�@�R���X�g���N�^

        public ReadOnlyComboBox()
        {
            this.components = new System.ComponentModel.Container();
            this.oldBackColor = this.BackColor;
        }

        #endregion

        #region�@Dispose ���\�b�h (Override)

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null)
                {
                    this.components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #endregion

        #region�@OnKeyDown ���\�b�h (Override)

        protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (!this.ReadOnly)
            {
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Delete:
                case Keys.Up:
                case Keys.Down:
                case Keys.PageUp:
                case Keys.PageDown:
                case Keys.F4:
                    {
                        e.Handled = true;
                        break;
                    }

                case Keys.Back:
                case Keys.V:
                case Keys.X:
                    {
                        this.keyPressHandled = true;
                        break;
                    }

                default:
                    {
                        this.keyPressHandled = false;
                        break;
                    }
            }
        }

        #endregion

        #region�@OnKeyPress (Override)

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (!this.ReadOnly)
            {
                this.keyPressHandled = false;
                return;
            }

            if (this.keyPressHandled)
            {
                e.Handled = true;
                this.keyPressHandled = false;
                return;
            }

            if (!char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }

            this.keyPressHandled = false;
        }

        #endregion

        #region�@ReadOnly �v���p�e�B

        private bool _ReadOnly;

        public bool ReadOnly
        {
            get
            {
                return this._ReadOnly;
            }

            set
            {
                this._ReadOnly = value;

                if (value)
                {
                    this.oldBackColor = this.BackColor;
                    this.BackColor = SystemColors.Control;
                    this.ContextMenu = new ContextMenu();
                    this.SetStyle(ControlStyles.Selectable, false);
                    this.SetStyle(ControlStyles.UserMouse, true);
                    this.UpdateStyles();
                    this.RecreateHandle();
                }
                else
                {
                    this.BackColor = this.oldBackColor;
                    this.ContextMenu = null;
                    this.SetStyle(ControlStyles.Selectable, true);
                    this.SetStyle(ControlStyles.UserMouse, false);
                    this.UpdateStyles();
                    this.RecreateHandle();
                }
            }
        }
        #endregion

    }
}
