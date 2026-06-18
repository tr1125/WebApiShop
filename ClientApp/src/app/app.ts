import { Component, signal } from '@angular/core';
import { RouterOutlet, RouterModule } from '@angular/router';
import { ChatComponent } from './components/chat/chat.component';
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterModule, ChatComponent],
  templateUrl: './app.html',
  styleUrls: ['./app.scss']
})
export class AppComponent {
  protected readonly title = signal('Room Design Studio');
}
