using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Plugin
{
    public class NumSort : System.Collections.IComparer
    {
        private SortOrder sort_order = SortOrder.Ascending;	// �\�[�g��(�����E�~��)
        private int column = 0;	// �\�[�g��
        private List<int> num_column; // ���l�Ŕ�r�����

        #region �v���p�e�B...
        public SortOrder Order
        {
            get { return sort_order; }
            set { sort_order = value; }
        }
        public int Column
        {
            get { return column; }
            set { column = value; }
        }
        #endregion

        public NumSort(params int[] list)
        {
            num_column = new List<int>(list);
        }
        // ��r���ʂ�Ԃ�
        public int Compare(object x, object y)
        {
            int ret = 0;
            // ��r�p���X�g�A�C�e���i�[�ϐ�
            ListViewItem sx = (ListViewItem)x;
            ListViewItem sy = (ListViewItem)y;

            // ��������r���A�l���i�[
            if (num_column.Contains(column))
            {
                // ���̏��������Ă����Ȃ���column���͈͊O���Ɠ{���Ă��܂����Ƃ�����
                // �Č������F
                // �u�ڍ׃��O�v�^�u�̍��̃��X�g�r���[�Łu����2�ȏ�̍��ځv��I�����C
                // �E�̃��X�g�r���[��2��ڈȍ~�̂ǂꂩ�̗�ɂ��ă\�[�g���s�������
                // ���̃��X�g�r���[�́u����2�ȏ�̍��ځv��I�����悤�Ƃ���Ɨ�����D
                // �����F�A�C�e���̒ǉ��̎d���ɒ���
                if (column < sx.SubItems.Count && column < sy.SubItems.Count)
                {
                    ret = int.Parse(sx.SubItems[column].Text) - int.Parse(sy.SubItems[column].Text);
                }
            }
            else
            {
                if (column < sx.SubItems.Count && column < sy.SubItems.Count)
                {
                    ret = string.Compare(sx.SubItems[column].Text, sy.SubItems[column].Text);
                }
            }

            // �~���̂Ƃ��͌��ʂ��t�]
            if (sort_order == SortOrder.Descending)
            {
                ret = -ret;
            }

            //���ʂ�Ԃ�
            return ret;
        }

        public void ChangeSortOrder()
        {
            if (Order == SortOrder.Descending)
            {
                Order = SortOrder.Ascending;
            }
            else
            {
                Order = SortOrder.Descending;
            }
        }
    }
}
