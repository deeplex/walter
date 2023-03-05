<script lang="ts">
	import { WalterHeaderList, WalterZaehlerList } from '$WalterComponents';
	import type { PageData } from './$types';

	export let data: PageData;
</script>

<WalterHeaderList url={data.url} title="Zähler" />
<WalterZaehlerList search rows={data.rows} />
