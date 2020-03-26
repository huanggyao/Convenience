import { Directive, Input, TemplateRef, ViewContainerRef } from '@angular/core';
import { StorageService } from 'src/app/core/services/storage.service';

@Directive({ selector: '[operate]' })
export class OperateDirective {

    constructor(
        private templateRef: TemplateRef<any>,
        private viewContainer: ViewContainerRef,
        private storageService: StorageService) { }

    @Input() set operate(identification: string) {

        let roles = this.storageService.UserRoles.split(',');
        let identifications = this.storageService.Identifycation.split(',');

        // 判断是否包含此元素的操作权
        let hasRight = roles.find(r => r == '超级管理员') ? true : identifications.find(i => i == identification);

        if (hasRight) {
            this.viewContainer.createEmbeddedView(this.templateRef);
        } else {
            this.viewContainer.clear();
        }
    }
}