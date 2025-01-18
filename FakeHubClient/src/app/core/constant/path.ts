import { environment } from '../../../environments/environment';

const api = environment.api;
const authApi = api + 'auth/';
const organizationApi = api + 'organization/';
const usersApi = api + 'users/';
const teamApi = organizationApi + 'team/';
const repositoryApi = api + 'repositories'

export const Path = {
  Register: authApi + 'register',
  Login: authApi + 'login',
  DockerImage: api + 'docker-images',
  RegisterAdmin: authApi + 'register/admin',
  ChangePassword: usersApi + 'change-password',
  ChangeEmail: usersApi + 'change-email',
  GetAllUsers: usersApi + 'all',
  Organization: organizationApi,
  ChangeUserBadge: usersApi + 'change-user-badge',
  OrganizationByUser: organizationApi + "user",
  Users: usersApi,
  AdminUsers: usersApi + "admins",
  Team: teamApi,
  OrganizationByUserIdName: organizationApi + "user/names",
  Repositories: repositoryApi
};

export function getProfilePath(username: string): string {
  return usersApi + username;
}

export function addMemberToOrganizationPath(name: string): string {
  return organizationApi + name + '/add-user';
}

export function deleteOrganizationMember(organizationName: string, username: string): string {
  return organizationApi + organizationName + '/delete-user/' + username;
}

export function deactivateOrganization(organizationName: string) : string {
  return organizationApi + organizationName + "/deactivate";
}

export function deleteTeamFromOrganization(organizationName: string, teamName: string) : string {
  return teamApi + organizationName + "/" + teamName;
}

export function deleteTeamMember(organizationName: string, teamName: string, username: string): string {
  return teamApi + organizationName + '/' + teamName + '/delete-user/' + username;
}