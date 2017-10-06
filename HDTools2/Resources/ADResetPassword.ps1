function GetWITUser
{
    $infoEntered = $args[0].ToLower()
    if ($infoEntered -like "w0*")
	{
		return $(GetWITUserByID $infoEntered)
	}
	elseif ($infoEntered -match ",")
	{
		return $(GetWITUserByLastFirst $infoEntered)
	}
	else
	{
		return $(GetWITUserByUsername $infoEntered)
	}
    #$adUser = If ($isWITID) {GetWITUserByID $infoEntered} else {GetWITUserByUsername $infoEntered}
    #return $adUser
}
function ResetUserPassword
{
    $user = $args[0]
    $WID = $user.Get_Item("EmployeeID")
    $WIDlastsix = $WID.Substring(3,6)
    $newPassword = "WIT1$"+$WIDlastsix
    $newPasswordSecure = ConvertTo-SecureString $newPassword -AsPlainText -Force
    Set-ADAccountPassword -identity $user -NewPassword $newPasswordSecure -Reset
	Set-ADUser -identity $user -ChangePasswordAtNextLogon $true
}
function GetWITUserByID 
{
    $id=$args[0]
    $user = Get-ADUser -Filter "EmployeeID -like ""$id""" -properties *
	if ($user)
	{
		return $user
	}
	return $null
}
function GetWITUserByLastFirst
{
	$lastFirst = $args[0].ToLower()
	$user = Get-ADUser -Filter {(Surname -like "*") -and (GivenName -like "*")} |
	? {$($lastFirst -match $_.Surname.ToLower())} |
	? {$($lastFirst -match $_.GivenName.ToLower())}
	return $user
}
function GetWITUserByUsername
{
    $username = $args[0].Replace('@wit.edu','')
    $user = Get-ADUser $username -properties *
    return $user
}