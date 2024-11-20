import { environment } from '../../../environments/environment';

const api = environment.api;

export const Path = {
  Register: api + 'auth/register',
  Login: api + 'auth/login',
  DockerImage: api + 'docker-images'
};

