export class DockerImage {
  title: string;
  description: string;
  likesCount: number;          
  downloadsCount: number;
  logoIcon: string;             
                    
  constructor(
    logoIcon: string,
    title: string,
    description: string,
    likesCount: number,
    downloadsCount: number
  ) {
    this.logoIcon = logoIcon;
    this.title = title;
    this.description = description;
    this.likesCount = likesCount;
    this.downloadsCount = downloadsCount;
  }
}
