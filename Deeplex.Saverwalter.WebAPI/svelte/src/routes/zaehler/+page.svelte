<script lang="ts">
	import { WalterHeader } from '../../components';
	import Zaehler from '../../components/lists/Zaehler.svelte';
	import { walter_get } from '../../services/utils';
	import type { ZaehlerEntry } from '../../types/zaehler.type';

	const rows: Promise<ZaehlerEntry[]> = walter_get('/api/zaehler');
</script>

<WalterHeader title="Zähler" />
<Zaehler search {rows} />
