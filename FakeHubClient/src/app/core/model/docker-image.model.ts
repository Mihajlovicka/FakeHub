export class DockerImage {
  logoIcon: string;             
  title: string;                
  badgeIcon: string;        
  description: string;         
  likesCount: number;          
  downloadsCount: number;   
  
  constructor(
    logoIcon: string,
    title: string,
    badgeIcon: string,
    description: string,
    likesCount: number,
    downloadsCount: number
  ) {
    this.logoIcon = logoIcon;
    this.title = title;
    this.badgeIcon = badgeIcon;
    this.description = description;
    this.likesCount = likesCount;
    this.downloadsCount = downloadsCount;
  }
}
