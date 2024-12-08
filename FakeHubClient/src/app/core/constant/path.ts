import { environment } from '../../../environments/environment';

const api = environment.api;
const authApi = api + 'auth/';
const organizationApi = api + 'organization/';
const usersApi = api + 'users/';
const teamApi = organizationApi + '/team';

export const Path = {
  Register: authApi + 'register',
  Login: authApi + 'login',
  DockerImage: api + 'docker-images',
  RegisterAdmin: authApi + 'register/admin',
  ChangePassword: usersApi + 'change-password',
  ChangeEmail: usersApi + 'change-email',
  Organization: organizationApi,
  ChangeUserBadge: usersApi + 'change-user-badge',
  OrganizationByUser: organizationApi + "user",
  Users: usersApi,
  AdminUsers: usersApi + "admins",
  Team: teamApi
};

export function getProfilePath(username: string): string {
  return usersApi + username;
}
