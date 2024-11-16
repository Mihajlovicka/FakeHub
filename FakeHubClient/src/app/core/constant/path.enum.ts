import { environment } from '../../../environments/environment';

const api = environment.api;
const authApi = api + 'auth/'

export const Path = {
  Register: authApi + 'register',
  Login: authApi + 'login',
  DockerImage: api + 'docker-images',
  RegisterAdmin: authApi + 'register/admin'
};

export function getProfilePath(username: string): string {
  return authApi + 'profile/' + username;
}
