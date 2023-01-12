import { Component, Input, ViewEncapsulation } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { Member } from 'src/app/models/member';
import { MembersService } from 'src/app/services/members.service';
import { PresenceService } from 'src/app/services/presence.service';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent {
  @Input() member: Member

  constructor(private memberService: MembersService, private tostr :ToastrService, public presenceService: PresenceService) { }

  ngOnInit(): void { }

  addLike(member: Member){
    this.memberService.addLike(member.userName).subscribe({
      next: ()=> this.tostr.success('You have liked ' + member.knownAs)
    })
  }
}
