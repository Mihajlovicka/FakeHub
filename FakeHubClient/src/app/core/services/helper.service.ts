import { Injectable } from "@angular/core";
import { format, isValid } from "date-fns";

@Injectable({
  providedIn: "root",
})
export class HelperService {
  public capitalizeFirstLetter(input: string): string {
    if (!input) {
      return "";
    }
    return input.charAt(0).toUpperCase();
  }

  public readImageBase64(file: File): Promise<string> {
    return new Promise((resolve) => {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        resolve(e.target.result);
      };
      reader.readAsDataURL(file);
    });
  }
}
