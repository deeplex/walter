<script lang="ts">
	import { WalterHeader } from '../../components';
	import Zaehler from '../../components/lists/Zaehler.svelte';
	import { walter_get } from '../../services/utils';
	import type { ZaehlerListEntry } from '../../types/zaehlerlist.type';

	const rows: Promise<ZaehlerListEntry[]> = walter_get('/api/zaehler');
</script>

<WalterHeader title="Zähler" />
<Zaehler search {rows} />
