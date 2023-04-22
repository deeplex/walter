import { walter_selection } from '$WalterServices/requests';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
  return {
    apiURL: `/api/betriebskostenrechnungen`,
    title: 'Neue Betriebskostenrechnung',
    betriebskostentypen: walter_selection.betriebskostentypen(fetch),
    umlagen: walter_selection.umlagen(fetch)
  };
};
