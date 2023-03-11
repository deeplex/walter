<script lang="ts">
	import { WalterHeaderList, WalterWohnungen } from '$WalterComponents';
	import type { PageData } from './$types';

	export let data: PageData;
</script>

<WalterHeaderList apiUrl={data.apiURL} title="Wohnungen" />
<WalterWohnungen kontakte={data.kontakte} search rows={data.rows} />
