<script lang="ts">
  import { page } from '$app/stores';
  import {
    WalterGrid,
    WalterHeaderNew,
    WalterBetriebskostenrechnung
  } from '$WalterComponents';
  import type { WalterBetriebskostenrechnungEntry } from '$WalterLib';
  import { convertDate } from '$WalterServices/utils';
  import type { PageData } from './$types';

  export let data: PageData;

  const entry: Partial<WalterBetriebskostenrechnungEntry> = {
    datum: convertDate(new Date())
  };
  let searchParams: URLSearchParams = new URL($page.url).searchParams;

  const betriebskostenTypId = searchParams.get('typ');
  if (betriebskostenTypId) {
    entry.typ = data.betriebskostentypen.find(
      (e) => +e.id === +betriebskostenTypId
    );
  }
  const umlageId = searchParams.get('umlage');
  if (umlageId) {
    entry.umlage = data.umlagen.find((e) => +e.id === +umlageId);
  }

  const jahr = searchParams.get('jahr');
  if (jahr) {
    entry.betreffendesJahr = +jahr;
  } else {
    entry.betreffendesJahr = new Date().getFullYear() - 1;
  }

  const betrag = searchParams.get('betrag');
  if (betrag) {
    entry.betrag = +betrag;
  }
</script>

<WalterHeaderNew apiURL={data.apiURL} {entry} title={data.title} />

<WalterGrid>
  <WalterBetriebskostenrechnung
    betriebskostentypen={data.betriebskostentypen}
    umlagen={data.umlagen}
    a={entry}
  />
</WalterGrid>
