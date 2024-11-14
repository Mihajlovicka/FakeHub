import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DockerImage } from './../model/docker-image.model';
import { Observable } from 'rxjs';
import { Path } from '../constant/path.enum';

@Injectable({
  providedIn: 'root'
})
export class DockerImageService {
  http: HttpClient = inject(HttpClient);

  getDockerImages(): Observable<DockerImage[]> {
    return this.http.get<DockerImage[]>(Path.DockerImage);
  }
}
