function GetWITUser
{
    $infoEntered = If ($args[0]) {$args[0].ToLower()} else {$(AskForInfo).ToLower()}
    $isWITID = $($infoEntered -like "w0*")
    $adUser = If ($isWITID) {GetWITUserByID $infoEntered} else {GetWITUserByUsername $infoEntered}
    return $adUser
}
function ResetUserPassword
{
    $user = $args[0]
    $WID = $user.Get_Item("EmployeeID")
    $WIDlastsix = $WID.Substring(3,6)
    $newPassword = "WIT1$"+$WIDlastsix
    $newPasswordSecure = ConvertTo-SecureString $newPassword -AsPlainText -Force
    Set-ADAccountPassword -identity $user -NewPassword $newPasswordSecure -Reset
}
function GetWITUserByID 
{
    $id=$args[0]
    $user = Get-ADUser -Filter "EmployeeID -like ""$id""" -properties $(PropsToGet)
    return $user
}
function GetWITUserByUsername
{
    $username = $args[0].Replace('@wit.edu','')
    $user = Get-ADUser $username -properties $(PropsToGet)
    return $user
}
function GetWITUserMain
{
    $user = GetWITUser $args[0]
    ShowMainForm $user $false
}
function PropsToGet { return @("DisplayName","Created","EmailAddress","EmployeeID","Enabled","LastBadPasswordAttempt","LastLogonDate","LockedOut","LogonCount","Modified","MemberOf","PasswordExpired","PasswordLastSet","Title") }
function PropsToGetShort { return @("DisplayName","EmailAddress","EmployeeID","PasswordExpired","PasswordLastSet","Title") }
function ShowMainForm
{
    $user = $args[0]
    $detailed = $args[1]
    Add-Type -AssemblyName System.Windows.Forms

    $Form = New-Object system.Windows.Forms.Form
    $Form.Text = "Form"
    $Form.TopMost = $true
    $Form.Width = 1500
    $Form.Height =  800
    
    #labelsContent = @($user.
    $row = 1
    $props = if ($detailed) {PropsToGet} else {PropsToGetShort}
    $props | Foreach-Object{
        $xCoordLeft = 10
        $xCoordRight = 50
        $yCoord = $row * 20
        $descLabel = New-Object system.windows.Forms.Label
        $descLabel.Text = $_
        $descLabel.AutoSize = $true
        $descLabel.Width = 80
        $descLabel.Height = 10
        $descLabel.location = new-object system.drawing.point($xCoordLeft,$($row*30))
        $descLabel.Font = "Microsoft Sans Serif,10"
        #$descLabel.WordWrap = $true
        $Form.controls.Add($descLabel)
        
        $row++
        #CastToHash $descLabel | Out-Host
        
        $dataLabel = New-Object system.windows.Forms.Label
        $dataLabel.Text = $user.$_
        $dataLabel.AutoSize = $true
        $dataLabel.Width = 80
        $dataLabel.Height = 10
        $dataLabel.location = new-object system.drawing.point($xCoordRight,$($row*30))
        $dataLabel.Font = "Microsoft Sans Serif,10"
        $Form.controls.Add($dataLabel)
        
        $row++
    }
    
    if (!($detailed))
    {
        $showDetailedFormButton = New-Object system.windows.Forms.Button
        $showDetailedFormButton.Text = "Show Detailed Info"
        $showDetailedFormButton.Width = 200
        $showDetailedFormButton.Height = 30
        $showDetailedFormButton.Add_Click({
            $Form.Close()
            $Form.Dispose()
            ShowMainForm $user $true
        })
        $showDetailedFormButton.location = new-object system.drawing.point(10,$($row*30))
        $showDetailedFormButton.Font = "Microsoft Sans Serif,10"
        $Form.controls.Add($showDetailedFormButton)
        $row++
    }
    $resetPasswordButton = New-Object system.windows.Forms.Button
    $resetPasswordButton.Text = "Reset password"
    $resetPasswordButton.Width = 200
    $resetPasswordButton.Height = 30
    $resetPasswordButton.Add_Click({
        if([System.Windows.Forms.MessageBox]::Show("Continue?", "Are you sure?",[System.Windows.Forms.MessageBoxButtons]::OKCancel) -eq "OK")
        {
            ResetUserPassword $user
        }
    })
    $resetPasswordButton.location = new-object system.drawing.point(10,$($row*30))
    $resetPasswordButton.Font = "Microsoft Sans Serif,10"
    $Form.controls.Add($resetPasswordButton)
    $row++
    
    $showPictureButton = New-Object system.windows.Forms.Button
    $showPictureButton.Text = "See User Image"
    $showPictureButton.Width = 200
    $showPictureButton.Height = 30
    $showPictureButton.Add_Click({
        ShowImage $user
    })
    $showPictureButton.location = new-object system.drawing.point(10,$($row*30))
    $showPictureButton.Font = "Microsoft Sans Serif,10"
    $Form.controls.Add($showPictureButton)
    $row++
    
    $Form.Activate()
    [void]$Form.ShowDialog()
}
function ShowImage
{
    
    $Form = New-Object system.Windows.Forms.Form
    $Form.Text = "Form"
    $Form.TopMost = $true
    $Form.Width = 1800
    $Form.Height =  1000
    $user = $args[0]
    $WID = $user.Get_Item("EmployeeID")
    $userPictureBox = New-Object system.windows.Forms.PictureBox
    $userPictureBox.ImageLocation = "http://photoid.wit.edu:8080/$WID.jpg"
    $userPictureBox.Height = 1000
    $userPictureBox.Width = 1800
    $userPictureBox.location = new-object system.drawing.point(0,0)
    $Form.controls.Add($userPictureBox)
    
    $Form.Activate()
    [void]$Form.ShowDialog()
}
function AskForInfo
{
    $global:returnVal = ""
    Add-Type -AssemblyName System.Windows.Forms

    $Form = New-Object system.Windows.Forms.Form
    $Form.Text = "Form"
    $Form.TopMost = $true
    $Form.Width = 200
    $Form.Height = 200
    
    $Form.KeyPreview = $True
    $Form.Add_KeyDown({if ($_.KeyCode -eq "Enter") 
        {$global:returnVal=$infoBox.Text;$Form.Close()}})

    $infoBox = New-Object system.windows.Forms.TextBox
    $infoBox.Width = 141
    $infoBox.Height = 20
    $infoBox.location = new-object system.drawing.point(10,20)
    $infoBox.Font = "Microsoft Sans Serif,10"
    $Form.controls.Add($infoBox)
    
    $closeButton = New-Object system.windows.Forms.Button
    $closeButton.Text = "Enter"
    $closeButton.Width = 60
    $closeButton.Height = 30
    $closeButton.Add_Click({
        $global:returnVal = $infoBox.Text
        $Form.Close()
    })
    $closeButton.location = new-object system.drawing.point(35,56)
    $closeButton.Font = "Microsoft Sans Serif,10"
    $Form.controls.Add($closeButton)
    

    $Form.Add_Shown({$Form.Activate()})
    [void]$Form.ShowDialog()
    return $global:returnVal
}
function CastToHash
{
    $hash = @{}
    $args[0].psobject.properties | Foreach {$hash[$_.Name] = $_.Value}
    return $hash
}