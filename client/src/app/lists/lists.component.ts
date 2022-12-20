import { Component } from '@angular/core';
import { Member } from '../models/member';
import { MembersService } from '../services/members.service';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css']
})
export class ListsComponent {
  members: Member[] ;
  predicate = 'liked';

  constructor(private memberService: MembersService){}

  ngOnInit(){
    this.loadLikes();
  }

  loadLikes(){
    this.memberService.getLikes(this.predicate).subscribe({
      next: response => {
        this.members = response
      }
    })
  }
}
