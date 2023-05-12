<script lang="ts">
  import {
    WalterGrid,
    WalterZaehler,
    WalterHeaderNew
  } from '$WalterComponents';
  import type { WalterZaehlerEntry } from '$WalterLib';
  import type { PageData } from './$types';

  export let data: PageData;

  const entry: Partial<WalterZaehlerEntry> = {};
</script>

<WalterHeaderNew apiURL={data.apiURL} {entry} title={data.title} />

<WalterGrid>
  <WalterZaehler
    wohnungen={data.wohnungen}
    umlagen={data.umlagen}
    zaehlertypen={data.zaehlertypen}
    a={entry}
  />
</WalterGrid>
