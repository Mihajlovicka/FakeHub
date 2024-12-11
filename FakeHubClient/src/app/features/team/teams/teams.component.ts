import { Component, Input } from "@angular/core";
import { Team } from "../../../core/model/team";
import { CommonModule, DatePipe } from "@angular/common";

@Component({
  selector: "app-teams",
  standalone: true,
  imports: [CommonModule],
  providers: [DatePipe],
  templateUrl: "./teams.component.html",
  styleUrl: "./teams.component.css",
})
export class TeamsComponent {
  @Input() teams: Team[] = [];
}
