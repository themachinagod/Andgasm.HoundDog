// =============================
// Email: info@ebenmonney.com
// www.ebenmonney.com/templates
// =============================

import { AppPage } from './app.po';
import { browser, logging } from 'protractor';

describe('workspace-project App', () => {
  let page: AppPage;

  beforeEach(() => {
    page = new AppPage();
  });

  it('should display application title: Andgasm.HoundDog.Web', () => {
    page.navigateTo();
    expect(page.getAppTitle()).toEqual('Andgasm.HoundDog.Web');
  });
});
