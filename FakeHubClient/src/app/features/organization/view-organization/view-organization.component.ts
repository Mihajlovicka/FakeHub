import { Component, inject } from "@angular/core";
import {
  Router,
  ActivatedRoute,
  RouterModule,
  Routes,
  RouterLinkActive,
} from "@angular/router";
import { Organization } from "../../../core/model/organization";
import { OrganizationService } from "../../../core/services/organization.service";
import { UserService } from "../../../core/services/user.service";
import { CommonModule } from "@angular/common";
import { MatIconModule } from "@angular/material/icon";
import { MatMenuModule } from "@angular/material/menu";
import { MatButtonModule } from "@angular/material/button";
import { MatTabsModule } from "@angular/material/tabs";
import { TeamsComponent } from "../../team/teams/teams.component";

@Component({
  selector: "app-view-organization",
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatMenuModule,
    MatButtonModule,
    RouterModule,
    MatTabsModule,
    TeamsComponent,
  ],
  templateUrl: "./view-organization.component.html",
  styleUrl: "./view-organization.component.css",
})
export class ViewOrganizationComponent {
  private readonly service: OrganizationService = inject(OrganizationService);
  private readonly activatedRoute: ActivatedRoute = inject(ActivatedRoute);
  private readonly userService: UserService = inject(UserService);
  private readonly router: Router = inject(Router);

  public organization: Organization = {
    name: "",
    description: "",
    imageBase64: "",
  };

  public isOwner(): boolean {
    return (
      this.organization.owner !== null &&
      this.organization.owner === this.userService.getUserName()
    );
  }

  public edit(): void {
    this.router.navigate(["/organization/edit", this.organization.name]);
  }

  public addTeam(): void {
    this.router.navigate(["/organization/team/add", this.organization.name]);
  }

  ngOnInit(): void {
    const name = this.activatedRoute.snapshot.paramMap.get("name");
    if (name) {
      this.service.getOrganization(name).subscribe({
        next: (organization: Organization) => {
          this.organization = organization;
        },
      });
    }
  }
}
