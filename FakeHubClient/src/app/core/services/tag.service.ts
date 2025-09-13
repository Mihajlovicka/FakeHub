import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { Path } from "../constant/path";
import { Artifact } from "../model/tag";

@Injectable({
  providedIn: "root",
})
export class TagService {
  private http: HttpClient = inject(HttpClient);

  public canUserDeleteTags(repositoryId: number): Observable<boolean> {
    return this.http.get<boolean>(`${Path.Tag}${repositoryId}/canUserDelete`);
  }

  public deleteTag(artifact: Artifact, repositoryId: number): Observable<Artifact[]> {
    return this.http.delete<Artifact[]>(`${Path.Tag}${repositoryId}`, { body: artifact });
  }
}
