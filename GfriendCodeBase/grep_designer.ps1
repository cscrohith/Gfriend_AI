Get-Content -Path "GFriendUI\MainForm.Designer.cs" | Select-String -Pattern "(targetDevice|labelTargetDevice|comboBox.*Device|buttonBrowse)" -Context 2,2
