<script lang="ts">
	import { WalterHeaderList, WalterZaehlerList } from '$WalterComponents';
	import type { PageData } from './$types';

	export let data: PageData;
</script>

<WalterHeaderList apiUrl={data.apiURL} title="Zähler" />
<WalterZaehlerList
	zaehlertypen={data.zaehlertypen}
	wohnungen={data.wohnungen}
	zaehler={data.zaehler}
	search
	rows={data.rows}
/>
