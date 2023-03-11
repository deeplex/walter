<script lang="ts">
	import { WalterAdressen, WalterHeaderList } from '$WalterComponents';
	import type { PageData } from './$types';

	export let data: PageData;
</script>

<WalterHeaderList apiUrl={data.apiURL} title="Adressen" />
<WalterAdressen search rows={data.rows} />
