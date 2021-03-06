﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MigAz.Azure;
using MigAz.Azure.Arm;
using System.Net;
using MigAz.Azure.Asm;
using MigAz.Azure.Interface;

namespace MigAz.UserControls
{
    public partial class ResourceGroupProperties : UserControl
    {
        private AsmToArmForm _ParentForm;
        private TreeNode _ResourceGroupNode;

        public ResourceGroupProperties()
        {
            InitializeComponent();
        }

        internal async Task Bind(AsmToArmForm parentForm, TreeNode resourceGroupNode)
        {
            _ParentForm = parentForm;
            _ResourceGroupNode = resourceGroupNode;

            ArmResourceGroup armResourceGroup = (ArmResourceGroup) _ResourceGroupNode.Tag;

            txtName.Text = armResourceGroup.Name;

            try
            {
                cboTargetLocation.Items.Clear();
                foreach (ArmLocation armLocation in await _ParentForm.AzureContextTargetARM.AzureRetriever.GetAzureARMLocations())
                {
                    cboTargetLocation.Items.Add(armLocation);
                }
            }
            catch (WebException)
            {
                // We are trying to load the ARM defined subscription locations above first; however, this as of Feb 24 2017, this ARM query
                // does not succeed (503 Forbidden) across all Azure Environments.  For example, it works in Azure Commercial, but Azure US Gov
                // is not yet update to support this call.  In the event the ARM location query fails, we will default to using ASM Location query.

                cboTargetLocation.Items.Clear();
                foreach (AsmLocation asmLocation in await _ParentForm.AzureContextTargetARM.AzureRetriever.GetAzureASMLocations())
                {
                    cboTargetLocation.Items.Add(asmLocation);
                }
            }

            if (armResourceGroup.Location != null)
            {
                foreach (ArmLocation armLocation in cboTargetLocation.Items)
                {
                    if (armLocation.Name == armResourceGroup.Location.Name)
                        cboTargetLocation.SelectedItem = armLocation;
                }
            }
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            TextBox txtSender = (TextBox)sender;

            ArmResourceGroup armResourceGroup = (ArmResourceGroup)_ResourceGroupNode.Tag;

            armResourceGroup.Name = txtSender.Text;
            _ResourceGroupNode.Text = armResourceGroup.GetFinalTargetName();
            _ResourceGroupNode.Name = armResourceGroup.Name;
        }

        private void cboTargetLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmbSender = (ComboBox)sender;

            ArmResourceGroup armResourceGroup = (ArmResourceGroup)_ResourceGroupNode.Tag;
            armResourceGroup.Location = (ILocation) cmbSender.SelectedItem;
        }
    }
}
