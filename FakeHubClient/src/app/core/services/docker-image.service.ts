import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DockerImage } from './../model/docker-image';
import { Observable, tap } from 'rxjs';
import { Path } from '../constant/path';

@Injectable({
  providedIn: 'root'
})
export class DockerImageService {
  private http: HttpClient = inject(HttpClient);

  getDockerImages(): Observable<DockerImage[]> {
    return this.http.get<DockerImage[]>(Path.DockerImage).pipe(
      tap((result: DockerImage[]) => {
        return result;
      })
    );
  }
}
