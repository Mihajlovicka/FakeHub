import { Component, inject } from "@angular/core";
import { Organization } from "../../../core/model/organization";
import { Router, RouterLink } from "@angular/router";
import { OrganizationService } from "../../../core/services/organization.service";

@Component({
  selector: "app-organizations",
  standalone: true,
  imports: [RouterLink],
  templateUrl: "./organizations.component.html",
  styleUrl: "./organizations.component.css",
})
export class OrganizationsComponent {
  public organizations: Organization[] = [];
  public readonly router: Router = inject(Router);
  public readonly organizationService: OrganizationService =
    inject(OrganizationService);

  ngOnInit(): void {
    this.organizationService
      .getByUser()
      .subscribe((organizations: Organization[]) => {
        this.organizations = organizations;
      });
  }
}
