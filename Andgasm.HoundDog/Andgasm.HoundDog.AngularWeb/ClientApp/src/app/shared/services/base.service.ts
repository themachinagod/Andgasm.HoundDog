import { throwError } from 'rxjs';

export abstract class BaseService {

  constructor() { }

  protected handleErrorArray(errorresponse: any) {
    try {
      debugger;
      var serverError = errorresponse.error;
      if (serverError.errors === undefined)
        return throwError({ "ServerError": "An unexpected error occurred while communicating with the server!" });
      else
        return throwError(serverError.errors);
    }
    catch (e) {
      return throwError({ "ServerError": "An unexpected error occurred while communicating with the server!" });
    }
  }
}
