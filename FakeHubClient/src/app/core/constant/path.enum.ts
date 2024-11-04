const service = 'http://localhost:5000/api/';

export enum Path {
  Register = service + 'auth/register',
  Login = service + 'auth/login',
}