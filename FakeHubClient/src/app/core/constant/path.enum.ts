import { environment } from '../../../environments/environment';

const api = environment.api;
const authApi = api + 'auth/'
const organizationApi = api + 'organization'

export const Path = {
  Register: authApi + 'register',
  Login: authApi + 'login',
  DockerImage: api + 'docker-images',
  RegisterAdmin: authApi + 'register/admin',
  ChangePassword: authApi + 'change-password',
  ChangeEmail: authApi + 'change-email',
  Organization: organizationApi,
  OrganizationByUser: organizationApi + "/user",
  ChangeUserBadge: authApi + 'change-user-badge'
};

export function getProfilePath(username: string): string {
  return authApi + 'profile/' + username;
}
