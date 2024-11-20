export interface ChangePasswordRequest {
    oldPassword: string;
    newPassword: string;
    newPasswordConfirm: string;
}