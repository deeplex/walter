<script lang="ts">
	import { WalterHeaderList, WalterWohnungen } from '$WalterComponents';
	import type { PageData } from './$types';

	export let data: PageData;
</script>

<WalterHeaderList url={data.url} title="Wohnungen" />
<WalterWohnungen search rows={data.rows} />
