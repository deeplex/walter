<script lang="ts">
	import { WalterKontakte, WalterHeaderList } from '$WalterComponents';
	import type { PageData } from './$types';

	export let data: PageData;
</script>

<WalterHeaderList apiUrl={data.apiURL} title="Kontakte" />
<WalterKontakte search rows={data.rows} />
