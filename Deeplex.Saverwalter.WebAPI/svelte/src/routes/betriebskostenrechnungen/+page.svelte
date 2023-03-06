<script lang="ts">
	import {
		WalterBetriebskostenrechnungen,
		WalterHeaderList
	} from '$WalterComponents';
	import type { PageData } from './$types';

	export let data: PageData;
</script>

<WalterHeaderList url={data.url} title="Betriebskostenrechnung" />
<WalterBetriebskostenrechnungen
	umlagen={data.umlagen}
	betriebskostentypen={data.betriebskostentypen}
	search
	rows={data.rows}
/>
