<script lang="ts">
	import { WalterHeader } from '../../components';
	import Betriebskostenrechnungen from '../../components/lists/Betriebskostenrechnungen.svelte';
	import { walter_get } from '../../services/utils';
	import type { BetriebskostenrechnungEntry } from '../../types/betriebskostenrechnung.type';

	const rows: Promise<BetriebskostenrechnungEntry[]> = walter_get(
		'/api/betriebskostenrechnungen'
	);
</script>

<WalterHeader title="Betriebskostenrechnung" />
<Betriebskostenrechnungen search {rows} />
