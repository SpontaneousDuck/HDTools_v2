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
    $user = Get-ADUser -Filter "EmployeeID -like ""$id""" -properties *
    return $user
}
function GetWITUserByUsername
{
    $username = $args[0].Replace('@wit.edu','')
    $user = Get-ADUser $username -properties *
    return $user
}